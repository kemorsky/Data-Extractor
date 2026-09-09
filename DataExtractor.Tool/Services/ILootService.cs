namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public interface ILootService
{
    CellLootData AnalyzeCell(
        ICellGetter cell,
        ILinkCache linkCache);
}