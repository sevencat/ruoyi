namespace sevencat.ruoyi.common.service;

public interface ISysDictDataApi
{
	Task<Dictionary<string, string>> SelectDictLabelMap(string dictType);
}