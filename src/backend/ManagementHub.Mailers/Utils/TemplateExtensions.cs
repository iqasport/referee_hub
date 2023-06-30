using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace ManagementHub.Mailers.Utils;

public static class Html
{
	public static HtmlString Raw(string source)
	{
		return new HtmlString(source);
	}
}
