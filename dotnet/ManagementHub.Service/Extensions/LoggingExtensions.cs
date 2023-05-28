using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Hangfire;
using Hangfire.Annotations;

namespace ManagementHub.Service.Extensions;

public static class LoggingExtensions
{
	public static string Enqueue<T>(this IBackgroundJobClient backgroundJobClient, ILogger logger, [InstantHandle][NotNull] Expression<Func<T, Task>> methodCall)
	{
		var jobId = backgroundJobClient.Enqueue(methodCall);
		var methodCallExpression = (MethodCallExpression)methodCall.Body;
		logger.LogInformation(0, "Enqueued background job: {methodCall} - id '{jobId}'",
			PrintMethodCall(methodCallExpression),
			jobId);
		return jobId;
	}

	private static string PrintMethodCall(MethodCallExpression methodCall)
	{
		var result = new StringBuilder();
		result.Append(methodCall.Method.Name);
		result.Append("(");

		var arguments = methodCall.Arguments;
		var parameters = methodCall.Method.GetParameters();
		for (int idx = 0; idx < parameters.Length; idx++)
		{
			if (parameters[idx].ParameterType == typeof(CancellationToken))
				continue;

			if (idx > 0) result.Append(", ");

			result.Append(parameters[idx].Name).Append(": ");
			var value = GetValueFromExpression(arguments[idx]);
			result.Append(value?.ToString() ?? "null");
		}

		result.Append(")");
		return result.ToString();
	}

	private static object? GetValueFromExpression(this Expression e)
	{
		switch (e)
		{
			case MemberExpression mem:
				{
					object? target = mem.Expression?.GetValueFromExpression();
					switch (mem.Member)
					{
						case FieldInfo field: return field.GetValue(target);
						case PropertyInfo prop: return prop.GetValue(target);
					}

					return null; // impossible
				}

			case ConstantExpression constant:
				return constant.Value;
			case null: return null;
			default: return new NotImplementedException();
		}
	}
}
