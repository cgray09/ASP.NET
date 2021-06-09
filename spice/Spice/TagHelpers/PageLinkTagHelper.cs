using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.TagHelpers
{
    [HtmlTargetElement("div", Attributes ="page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound] // isnt the element your setting the tag for
        public ViewContext ViewContext { get; set; }

        // these are like your asp-for properties
        public PagingInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; } // see if the classes are enabled or not to set to tag helper
        public string PageClass { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            TagBuilder result = new TagBuilder("div");

            for(int i=1;i<=PageModel.totalPage;i++)
            {
                TagBuilder tag = new TagBuilder("a");
                // gets it from the model which is set in the action
                string url = PageModel.urlParam.Replace(":", i.ToString()); // replace : w/ the current page
                tag.Attributes["href"] = url; // so clicking on the link redirects to the proper page by page number (click 1 got to page 1, ect)
                if(PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }
                tag.InnerHtml.Append(i.ToString()); // adds 1, 2, 3, 4 to our pagination
                result.InnerHtml.AppendHtml(tag);
            }

            // display the main div now that it has been appended
            output.Content.AppendHtml(result.InnerHtml);

        }

    }
}
