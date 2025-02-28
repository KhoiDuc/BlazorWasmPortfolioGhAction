using AutoMapper;
using BlazorWasmPortfolioGhAction.Shared.Model;
using BlazorWasmPortfolioGhAction.ViewModels;

namespace BlazorWasmPortfolioGhAction;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<DiagramParametersViewModel, DiagramParametersModel>()
            .ForMember(destination => destination.Width,
                option => option.MapFrom(source => source.Width == null
                ? DiagramParametersViewModel.MinPositiveInt : int.Parse(source.Width)))
            .ForMember(destination => destination.Height,
                option => option.MapFrom(source => source.Height == null
                ? DiagramParametersViewModel.MinPositiveInt : int.Parse(source.Height)))
            .ForMember(destination => destination.GridStep,
                option => option.MapFrom(source => source.GridStep == null
                ? DiagramParametersViewModel.MinPositiveInt : int.Parse(source.GridStep)))
            .ReverseMap();
    }
}