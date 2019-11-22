#tool nuget:?package=Codecov&version=1.5.0
#addin nuget:?package=Cake.Codecov&version=0.6.0
#addin nuget:?package=Cake.Coverlet&version=2.3.4

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

public class MyBuildData
{
   public string Configuration { get; }

   public ConvertableDirectoryPath ArtifactsDirectory { get; }

   public ConvertableDirectoryPath CoverageDirectory { get; }

   public ConvertableDirectoryPath CoverageReportDirectory { get; }

   public ConvertableDirectoryPath TestResultsDirectory { get; }

   public IReadOnlyList<ConvertableDirectoryPath> BuildDirs { get; }

   public DotNetCoreBuildSettings BuildSettings { get; }

   public DotNetCoreTestSettings TestSettings { get; }

   public DotNetCorePackSettings PackSettings { get; }

   public MyBuildData(
      string configuration,
      ConvertableDirectoryPath artifactsDirectory,
      ConvertableDirectoryPath coverageDirectory,
      ConvertableDirectoryPath coverageReportDirectory,
      ConvertableDirectoryPath testResultsDirectory,
      IReadOnlyList<ConvertableDirectoryPath> buildDirectories)
   {
      Configuration = configuration;
      ArtifactsDirectory = artifactsDirectory;
      CoverageDirectory = coverageDirectory;
      CoverageReportDirectory = coverageReportDirectory;
      TestResultsDirectory = testResultsDirectory;
      BuildDirs = buildDirectories;

      BuildSettings = new DotNetCoreBuildSettings {
         Configuration = configuration,
         NoRestore = true,
         ArgumentCustomization = args => args.Append("/property:WarningLevel=0") // Until Warnings are fixed in StyleCop
      };

      TestSettings = new DotNetCoreTestSettings {
            Configuration = configuration,
            NoBuild = true,
            Verbosity = DotNetCoreVerbosity.Quiet,
            ArgumentCustomization = args =>
                args.Append("--logger:trx")
        };

      PackSettings = new DotNetCorePackSettings
      {
         OutputDirectory = ArtifactsDirectory,
         NoBuild = true,
         Configuration = Configuration,
      };
   }
}

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup<MyBuildData>(setupContext =>
{
   return new MyBuildData(
      configuration: Argument("configuration", "Release"),
      artifactsDirectory: Directory("./artifacts/"),
      coverageDirectory: Directory("./coverage-results"),
      coverageReportDirectory: Directory("./coverage-report"),
      testResultsDirectory: Directory("./test/Stubble.Helpers.Test/TestResults"),
      buildDirectories: new List<ConvertableDirectoryPath> {
         Directory("./src/Stubble.Helpers/bin"),
         Directory("./test/Stubble.Helpers.Test/bin"),
      });
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does<MyBuildData>((data) =>
{
    foreach (var dir in data.BuildDirs)
    {
        CleanDirectory(dir + Directory(data.Configuration));
    }
    CleanDirectory(data.ArtifactsDirectory);
    CleanDirectory(data.CoverageDirectory);
    CleanDirectory(data.CoverageReportDirectory);
    CleanDirectory(data.TestResultsDirectory);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
   DotNetCoreRestore("./Stubble.Helpers.sln");
});

Task("Build")
    .IsDependentOn("Restore")
    .Does<MyBuildData>((data) =>
{
   DotNetCoreBuild("./Stubble.Helpers.sln", data.BuildSettings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does<MyBuildData>((data) =>
{
    var coverletSettings = new CoverletSettings {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.opencover | CoverletOutputFormat.cobertura,
        CoverletOutputDirectory = data.CoverageDirectory,
        CoverletOutputName = $"results-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss-FFF}"
    };

    DotNetCoreTest("./test/Stubble.Helpers.Test/", data.TestSettings, coverletSettings);
});

Task("Pack")
    .WithCriteria(!BuildSystem.IsRunningOnTravisCI)
    .IsDependentOn("Test")
    .Does<MyBuildData>((data) =>
{
    DotNetCorePack("./src/Stubble.Helpers/Stubble.Helpers.csproj", data.PackSettings);
});

Task("CodeCov")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsRunningOnAzurePipelinesHosted && IsRunningOnWindows())
    .Does(() =>
{
    var coverageFiles = GetFiles("./coverage-results/*.opencover.xml")
        .Select(f => f.FullPath)
        .ToArray();
    var token = EnvironmentVariable("CODECOV_REPO_TOKEN");

    var settings = new CodecovSettings
    {
        Token = token,
        Files = coverageFiles
    };

    // Upload coverage reports.
    Codecov(settings);
});

Task("Default")
   .IsDependentOn("CodeCov");

RunTarget(target);