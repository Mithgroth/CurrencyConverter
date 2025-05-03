using Unit.Scenarios;

namespace Unit.Formatters;

public class CurrencyConversionFormatter : ArgumentDisplayFormatter
{
    public override bool CanHandle(object? value) => value is CurrencyConversionScenarios.Scenario;

    public override string FormatValue(object? value)
    {
        var s = (CurrencyConversionScenarios.Scenario)value!;
        return $"{s.Name} from {s.Source.Code} to {s.Target.Code}, {s.Amount} @ {s.Snapshot?.Date:yyyy-MM-dd}";
    }
}
