#pragma checksum "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "453f086513e2a0470afb397e9e301ca251bbbf55"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator.Pages_Diplomarbeit_Moderator_Moderator), @"mvc.1.0.razor-page", @"/Pages/Diplomarbeit/Moderator/Moderator.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.RazorPageAttribute(@"/Pages/Diplomarbeit/Moderator/Moderator.cshtml", typeof(Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator.Pages_Diplomarbeit_Moderator_Moderator), null)]
namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\_ViewImports.cshtml"
using Managementsystem_Classconferences;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"453f086513e2a0470afb397e9e301ca251bbbf55", @"/Pages/Diplomarbeit/Moderator/Moderator.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"6723dde29bfddb35b3b083cdd4e565f225c02a35", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Diplomarbeit_Moderator_Moderator : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("method", "post", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 5 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
     if (Model.State_of_Conference != "completed")
    {



#line default
#line hidden
            BeginContext(99, 68, true);
            WriteLiteral("    <table>\r\n        <thead>\r\n            <tr>\r\n                <th>");
            EndContext();
            BeginContext(168, 23, false);
#line 12 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
               Write(Model.Name_CurrentClass);

#line default
#line hidden
            EndContext();
            BeginContext(191, 63, true);
            WriteLiteral("</th>\r\n            </tr>\r\n        </thead>\r\n        <tbody>\r\n\r\n");
            EndContext();
#line 17 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
             foreach (var teacher in Model.CurrentClass.Teachers)
            {

#line default
#line hidden
            BeginContext(336, 46, true);
            WriteLiteral("                <tr>\r\n                    <td>");
            EndContext();
            BeginContext(383, 18, false);
#line 20 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
                   Write(teacher.Name_Short);

#line default
#line hidden
            EndContext();
            BeginContext(401, 30, true);
            WriteLiteral("</td>\r\n                </tr>\r\n");
            EndContext();
#line 22 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
            }

#line default
#line hidden
            BeginContext(446, 36, true);
            WriteLiteral("        </tbody>\r\n    </table>\r\n    ");
            EndContext();
            BeginContext(482, 124, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "0653039697bb49b5bb3b9c58e724c880", async() => {
                BeginContext(502, 56, true);
                WriteLiteral("\r\n        <button type=\"submit\" class=\"btn btn-default\">");
                EndContext();
                BeginContext(559, 25, false);
#line 26 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
                                                 Write(Model.State_of_Conference);

#line default
#line hidden
                EndContext();
                BeginContext(584, 15, true);
                WriteLiteral("</button>\r\n    ");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper.Method = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(606, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 28 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
    }
    else
    {

#line default
#line hidden
            BeginContext(632, 42, true);
            WriteLiteral("        <h1>Konferenz abgeschlossen</h1>\r\n");
            EndContext();
#line 32 "C:\Users\Christian\Desktop\git\Diplomarbeit\Managementsystem_Classconferences\Managementsystem_Classconferences\Pages\Diplomarbeit\Moderator\Moderator.cshtml"
    }

#line default
#line hidden
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<ModeratorModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ModeratorModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ModeratorModel>)PageContext?.ViewData;
        public ModeratorModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
