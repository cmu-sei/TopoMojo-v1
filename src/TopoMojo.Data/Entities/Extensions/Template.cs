namespace TopoMojo.Data.Entities.Extensions
{
    public static class TemplateExtensions
    {
        public static bool IsLinked(this Template template)
        {
            return template.ParentId != null;
        }
    }
}