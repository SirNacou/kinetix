// Program.cs

using Kinetix.Core;
using Kinetix.Core.Compiler;
using Kinetix.Core.Parsing;

var machine = new MachineContext
{
	Speed = 200,
	Status = "Running",
	Temperature = 95.5m
};

// A "Maintenance Trigger" rule:
// "If Temp is high AND the machine is stopped"
string formula = "if(Speed > 100 && Status == 'Running', 100, 0)";

Console.WriteLine($"Formula: {formula}");

var engine = new KinetixEngine();

var parseResult = engine.Execute<MachineContext, bool>(formula, machine);

var output = parseResult.Match(
	Succ: result => $"Success: {result}",
	Fail: ex => $"Syntax Error: {ex.Message}"
);

Console.WriteLine(output);

// This is the "Schema" for your machine
public class MachineContext
{
	public decimal Speed { get; set; }
	public string Status { get; set; } = ""; // Default to empty to avoid null issues
	public decimal Temperature { get; set; }
}