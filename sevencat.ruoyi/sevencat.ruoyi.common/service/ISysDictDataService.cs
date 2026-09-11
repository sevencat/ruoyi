namespace sevencat.ruoyi.common.service;

public interface ISysDictDataService
{
	Task<Dictionary<string, string>> SelectDictLabelMap(string dictType);
}