namespace Kortos.Web.Models;

public class EmbedModel
{
    public EmbedModel(int id, string title, string innerContentHtml, Style style)
    {
        Id = id;
        Title = title;
        InnerContentHtml = innerContentHtml;
        Style = style;
    }
    public int Id { get; set; }
    public string Title { get; set; }
    public string InnerContentHtml { get; set; }
    public Style Style { get; set; }
}