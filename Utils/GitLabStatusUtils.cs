using Meziantou.GitLab;

namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// GitLab pipeline/job status rendering helpers.
/// Consolidates duplicated <c>IsCompleted</c>/<c>GetStatusBadgeClass</c>/<c>GetStatusIcon</c>
/// from <c>GitLabRunPipeline.razor</c> and <c>GitLabRetryJobs.razor</c>.
/// </summary>
public static class GitLabStatusUtils
{
    // --- PipelineStatus ---

    public static bool IsCompleted(PipelineStatus status) =>
        status is PipelineStatus.Canceled or PipelineStatus.Failed or PipelineStatus.Success;

    public static string GetStatusBadgeClass(PipelineStatus status) =>
        BadgeClassFor(status.ToString());

    public static string GetStatusIcon(PipelineStatus status) =>
        IconFor(status.ToString());

    // --- JobStatus ---

    public static bool IsCompleted(JobStatus status) =>
        status is JobStatus.Canceled or JobStatus.Failed or JobStatus.Success;

    public static string GetStatusBadgeClass(JobStatus status) =>
        BadgeClassFor(status.ToString());

    public static string GetStatusIcon(JobStatus status) =>
        IconFor(status.ToString());

    // --- Shared mapping by enum name ---

    private static string BadgeClassFor(string name) => name switch
    {
        "Success" => "gitlab-status-success",
        "Failed" => "gitlab-status-failed",
        "Running" => "gitlab-status-running",
        "Pending" or "Created" => "gitlab-status-pending",
        "Canceled" or "Manual" => "gitlab-status-canceled",
        "Skipped" => "gitlab-status-skipped",
        _ => "gitlab-status-default",
    };

    private static string IconFor(string name) => name switch
    {
        "Success" => "bi-check-circle-fill",
        "Failed" => "bi-x-circle-fill",
        "Running" => "bi-arrow-repeat",
        "Pending" or "Created" => "bi-clock-fill",
        "Canceled" => "bi-slash-circle-fill",
        "Manual" => "bi-hand-index-fill",
        "Skipped" => "bi-skip-forward-fill",
        _ => "bi-question-circle-fill",
    };
}