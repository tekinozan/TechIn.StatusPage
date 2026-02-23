using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;
using TechIn.StatusPage.UI.Rendering;
using TechIn.StatusPage.UI.Rendering.Hydrators;

namespace TechIn.StatusPage.UI;

public static class StatusPageRenderer
{
    public static string Render(StatusPageResponse data, StatusPageOptions options)
    {
        // 1. Resolve the correct hydrator
        TemplateHydratorBase hydrator = options.Template switch
        {
            StatusPageTemplate.Classic => new ClassicHydrator(),
            StatusPageTemplate.Axiom => new AxiomHydrator(),
            StatusPageTemplate.Pulse => new PulseHydrator(),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Template), options.Template, "Unknown template")
        };

        // 2. Build the dictionary using the base class logic
        var placeholders = hydrator.Build(data, options);

        // 3. Hydrate the HTML
        var templateName = options.Template.ToString();
        var html = TemplateEngine.Load(templateName);

        return TemplateEngine.Hydrate(html, placeholders);
    }
}