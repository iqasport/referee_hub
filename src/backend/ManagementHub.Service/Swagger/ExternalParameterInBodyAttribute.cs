namespace ManagementHub.Service.Swagger;

public class ExternalParameterInBodyAttribute : Attribute
{
	public string Name { get; }
	public string MediaType { get; set; } = "application/json";

	public ExternalParameterInBodyAttribute(string name)
	{
		this.Name = name;
	}
}
