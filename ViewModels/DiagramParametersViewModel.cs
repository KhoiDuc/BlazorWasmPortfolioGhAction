using System.ComponentModel.DataAnnotations;

namespace BlazorWasmPortfolioGhAction.ViewModels;

public class DiagramParametersViewModel
{
    public const int MinPositiveInt = 1;
    public const int MaxSize = 10000;

    public const string SideShouldBeDefinedMessageEnd = " must be defined";
    public const string SideShouldBePositiveIntMessageEnd = " must be a positive integer";
    public const string SideShouldBeInRangeMessageEnd = " must be in the range from 1 to 10000";

    public const string WidthCaption = "Width";
    public const string HeightCaption = "Height";
    public const string GridStepCaption = "Grid Step";

    [Required(ErrorMessage = WidthCaption + SideShouldBeDefinedMessageEnd)]
    [RegularExpression("([1-9][0-9]*)", ErrorMessage = WidthCaption
        + SideShouldBePositiveIntMessageEnd)]
    [Range(MinPositiveInt, MaxSize, ErrorMessage = WidthCaption + SideShouldBeInRangeMessageEnd)]
    public string? Width { get; set; } = "400";

    [Required(ErrorMessage = HeightCaption + SideShouldBeDefinedMessageEnd)]
    [RegularExpression("([1-9][0-9]*)", ErrorMessage = HeightCaption
        + SideShouldBePositiveIntMessageEnd)]
    [Range(MinPositiveInt, MaxSize, ErrorMessage = HeightCaption + SideShouldBeInRangeMessageEnd)]
    public string? Height { get; set; } = "300";

    public bool ShowGrid { get; set; }

    [Required(ErrorMessage = GridStepCaption + " must be defined")]
    [RegularExpression("([1-9][0-9]*)", ErrorMessage = GridStepCaption
        + " must be a positive integer")]
    [Range(MinPositiveInt, 100, ErrorMessage = GridStepCaption
        + " must be in the range from 1 to 100")]
    public string? GridStep { get; set; } = "15";
}