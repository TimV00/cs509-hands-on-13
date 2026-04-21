// build.cake
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var projectFile = "./app.sln";
var docsDir = "./docs";
var siteDir = "./docs/_site";

var isAppVeyor = AppVeyor.IsRunningOnAppVeyor;

// ── Tasks ────────────────────────────────────────────────

Task("Clean")
    .Does(() =>
    {
        DotNetClean(projectFile);
        CleanDirectory(siteDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore(projectFile);
    });

Task("Format")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetTool("format", new DotNetToolSettings
        {
            ArgumentCustomization = args => args
                .Append("./app.sln")
                .Append("--verify-no-changes")
        });
    });

Task("Build-Debug")
    .IsDependentOn("Format")
    .Does(() =>
    {
        DotNetBuild(projectFile, new DotNetBuildSettings
        {
            Configuration = "Debug",
            NoRestore = true
        });
    });

Task("Build-Release")
    .IsDependentOn("Format")
    .Does(() =>
    {
        DotNetBuild(projectFile, new DotNetBuildSettings
        {
            Configuration = "Release",
            NoRestore = true
        });
    });

Task("Analyze")
    .IsDependentOn("Build-Release")
    .Does(() =>
    {
        DotNetBuild(projectFile, new DotNetBuildSettings
        {
            Configuration = "Release",
            NoRestore = true,
            ArgumentCustomization = args => args
                .Append("/p:RunAnalyzersDuringBuild=true")
                .Append("/p:AnalysisMode=All")
                .Append("/p:TreatWarningsAsErrors=true")
        });
    });

Task("Test")
    .IsDependentOn("Build-Debug")
    .IsDependentOn("Build-Release")
    .Does(() =>
    {
        DotNetTest(projectFile, new DotNetTestSettings
        {
            Configuration = "Release",
            NoRestore = true,
            NoBuild = true,
            Loggers = new[] { "trx" },
            ResultsDirectory = "./TestResults"
        });

        if (isAppVeyor)
        {
            foreach (var result in GetFiles("./TestResults/*.trx"))
                AppVeyor.UploadTestResults(result, AppVeyorTestResultsType.MSTest);
        }
    });

Task("Docs")
    .IsDependentOn("Build-Release")
    .Does(() =>
    {
        StartProcess("docfx", new ProcessSettings
        {
            Arguments = $"metadata {docsDir}/docfx.json"
        });

        StartProcess("docfx", new ProcessSettings
        {
            Arguments = $"build {docsDir}/docfx.json"
        });
    });

Task("Publish-Docs")
    .IsDependentOn("Docs")
    .Does(() =>
    {
        Zip(siteDir, "./docs-site.zip");

        if (isAppVeyor)
            AppVeyor.UploadArtifact("./docs-site.zip");
    });

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Analyze");

Task("Full")
    .IsDependentOn("Test")
    .IsDependentOn("Analyze")
    .IsDependentOn("Publish-Docs");

// ── Entry Point ──────────────────────────────────────────

RunTarget(target);
