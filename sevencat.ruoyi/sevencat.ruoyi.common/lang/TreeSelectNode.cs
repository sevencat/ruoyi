using System.Text.Json.Serialization;

namespace sevencat.ruoyi.common.lang;

public class TreeSelectNode<T>
{
	public long Id { get; set; }
	public long ParentId { get; set; }
	public string Label { get; set; }
	public int Weight { get; set; }
	public bool Disabled { get; set; }
	
	[JsonIgnore]
	public T Data { get; set; }
	
	public List<TreeSelectNode<T>> Children { get; set; }
}