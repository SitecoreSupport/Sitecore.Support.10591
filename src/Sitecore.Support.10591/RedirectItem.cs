using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.XA.Foundation.Multisite;
using Sitecore.XA.Foundation.Multisite.Extensions;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Support.XA.Feature.Redirects.Pipelines.HttpRequestBegin
{
  public class RedirectItem : HttpRequestProcessor
  {
    public override void Process(HttpRequestArgs args)
    {
      var item = Context.Item;
      if (Context.Database == null || item == null)
      {
        return;
      }

      if (!Context.Site.IsSxaSite())
      {
        return;
      }

      if (item.InheritsFrom(Sitecore.XA.Feature.Redirects.Templates.Redirect.ID))
      {
        var redirectUrl = GetRedirectUrl();
        if (!redirectUrl.IsNullOrEmpty())
        {
          args.Context.Response.Redirect(redirectUrl, true);
          args.AbortPipeline();
        }
      }
    }

    protected virtual string GetRedirectUrl()
    {
      LinkField linkField = Context.Item.Fields[Sitecore.XA.Feature.Redirects.Templates.Redirect.Fields.RedirectUrl];
      string redirectUrl = null;
      if (linkField != null)
      {
        if (linkField.IsInternal && linkField.TargetItem != null)
        {
         SiteInfo site = ServiceLocator.ServiceProvider.GetService<ISiteInfoResolver>().GetSiteInfo(linkField.TargetItem);
          UrlOptions options = UrlOptions.DefaultOptions;
          options.Site = SiteContextFactory.GetSiteContext(site.Name);
          options.AlwaysIncludeServerUrl = true;

          redirectUrl = LinkManager.GetItemUrl(linkField.TargetItem, options) + (string.IsNullOrEmpty(linkField.QueryString) ? "" : "?" + linkField.QueryString);
        }
        else if (linkField.IsMediaLink && linkField.TargetItem != null)
        {
          redirectUrl = ((MediaItem)linkField.TargetItem).GetMediaUrl();
        }
        else
        {
          redirectUrl = linkField.Url;
        }
      }
      return redirectUrl;
    }
  }
}