using System;
using Microsoft.AspNetCore.Components;

namespace CvShortlist.SelfHosted.Components.Layout;

public partial class Pagination
{
	[Parameter] public int CurrentPage { get; set; }
	[Parameter] public int TotalPages { get; set; }
	[Parameter] public EventCallback<int> OnPageChanged { get; set; }

	private int _startPage;
	private int _endPage;

	protected override void OnParametersSet()
	{
		_startPage = Math.Max(1, CurrentPage - 5);
		_endPage = Math.Min(TotalPages, CurrentPage + 5);
	}
}
