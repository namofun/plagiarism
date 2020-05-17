using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// When <c>ViewData.Contains(asp-viewdata-key)</c> or <c>User.IsInRoles(asp-in-roles)</c> is not satisfied, suppress the tag output.
    /// </summary>
    [HtmlTargetElement(Attributes = ViewDataKey)]
    [HtmlTargetElement(Attributes = InRolesKey)]
    [HtmlTargetElement(Attributes = ConditionKey)]
    public class DisplayWhenTagHelper : TagHelper
    {
        private const string ViewDataKey = "asp-viewdata-key";
        private const string InRolesKey = "asp-in-roles";
        private const string ConditionKey = "asp-show-if";

#pragma warning disable CS8618
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
#pragma warning restore CS8618

        /// <summary>
        /// The required ViewData keys
        /// </summary>
        [HtmlAttributeName(ViewDataKey)]
        public string? Key { get; set; }

        /// <summary>
        /// The required user roles
        /// </summary>
        [HtmlAttributeName(InRolesKey)]
        public string? Roles { get; set; }

        /// <summary>
        /// The display requirement
        /// </summary>
        [HtmlAttributeName(ConditionKey)]
        public bool ShowIf { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            bool suppress = ShowIf;
            if (Key != null && ViewContext.ViewData.ContainsKey(Key))
                suppress = false;
            if (Roles != null && ViewContext.HttpContext.User.IsInRoles(Roles))
                suppress = false;
            if (suppress)
                output.SuppressOutput();
        }
    }

    /// <summary>
    /// Razor tag block without wrapping in output but in code.
    /// </summary>
    [HtmlTargetElement("razor")]
    public class EmptyWrapperTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagName = null;
        }
    }
}
