using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public class BTConsoleCommand
{
	private List<string> commandNamespace;
	private BTCommandInput commandInput;
	private string helpText;
	private Action<BTCommandInput> executeCallback;

	public BTCommandInput GetCommandInput() {
		return commandInput;
	}

	public string GetInfoText() {
		return helpText;
	}

	public BTConsoleCommand(List<string> commandNamespace, BTCommandInput commandInput, string helpText, Action<BTCommandInput> executeCallback) {
		this.commandNamespace = commandNamespace;
		this.commandInput = commandInput;
		this.helpText = helpText;
		this.executeCallback = executeCallback;
	}

	public static bool InputPartiallyMatches(string input, string target) {
		char[] inputChars = input.ToCharArray();
		int inputCharsLength = inputChars.GetLength(0);
		char[] targetChars = target.ToCharArray();
		int targetCharsLength = targetChars.GetLength(0);
		int targetIndex = 0;
		for(int inputIndex = 0; inputIndex < inputCharsLength; inputIndex++) {
			char inputChar = char.ToUpperInvariant(inputChars[inputIndex]);
			while(inputChar != char.ToUpperInvariant(targetChars[targetIndex])) {
				targetIndex++;
				if(targetIndex >= targetCharsLength) {
					// unable to find matching character in target
					return false;
				}
			}
			targetIndex++;
			if(targetIndex >= targetCharsLength && (inputIndex + 1) < inputCharsLength) {
				// reached end of target string, but still have input chars to check
				return false;
			}
		}
		return true;
	}

	public int ShowAsSuggestion(List<string> input) {
		int inputCount = input.Count;
		int namespaceCount = commandNamespace.Count;
		int totalArgumentCount = commandInput.TotalArgumentCount();
		if(namespaceCount + totalArgumentCount < inputCount) {
			// input is too long to ever match this
			return 0;
		}
		int searchableKeywords = Mathf.Min(namespaceCount, inputCount);
		for(int index = 0; index < searchableKeywords; index++) {
			string inputString = input[index];
			string namespaceString = commandNamespace[index];
			if(!InputPartiallyMatches(inputString, namespaceString)) {
				return 0;
			}
		}
		return searchableKeywords;
	}

	public string Autocomplete(List<string> input) {
		int inputCount = input.Count;
		int namespaceCount = commandNamespace.Count;
		string namespaceToString = String.Join(" ", commandNamespace);
		if(inputCount <= namespaceCount) {
			// namespace incomplete, just complete namespace
			return namespaceToString;
		} else {
			StringBuilder resultBuilder = new StringBuilder(namespaceToString);
			for(int index = namespaceCount; index < inputCount; index++) {
				resultBuilder.Append(input[index]);
			}
			return resultBuilder.ToString();
		}
	}

	public bool IsFullNamespaceMatching(List<string> input) {
		int inputCount = input.Count;
		int namespaceCount = commandNamespace.Count;
		int totalArgumentCount = commandInput.TotalArgumentCount();
		if(inputCount < namespaceCount || inputCount > namespaceCount + totalArgumentCount) {
			return false;
		}
		for(int index = 0; index < namespaceCount; index++) {
			string inputString = input[index];
			string namespaceString = commandNamespace[index];
			if(!InputPartiallyMatches(inputString, namespaceString)) {
				return false;
			}
		}
		return true;
	}

	public bool IsCommandMatching(List<string> input) {
		int inputCount = input.Count;
		int namespaceCount = commandNamespace.Count;
		int totalArgumentCount = commandInput.TotalArgumentCount();
		int requiredArgumentCount = commandInput.RequiredArgumentCount();
		if(namespaceCount + requiredArgumentCount > inputCount) {
			// input too short to match this command
			return false;
		}
		if(namespaceCount + totalArgumentCount < inputCount) {
			// input too long to match this command
			return false;
		}
		for(int namespaceIndex = 0; namespaceIndex < namespaceCount; namespaceIndex++) {
			string inputString = input[namespaceIndex];
			string namespaceString = commandNamespace[namespaceIndex];
			if(!InputPartiallyMatches(inputString, namespaceString)) {
				return false;
			}
		}
		return true;
	}

	public void Execute(List<string> input) {
		commandInput.ParseArguments(input, commandNamespace.Count);
		executeCallback.Invoke(commandInput);
	}

	public string GetNamespaceString() {
		return String.Join(" ", commandNamespace);
	}

	public string ShortHelp() {
		StringBuilder builder = new StringBuilder("");
		builder.Append(GetNamespaceString());
		builder.Append(" ").Append(commandInput.GetArgumentTemplate());
		builder.Append(" - ").Append(helpText);
		return builder.ToString();
	}

	public string Help() {
		StringBuilder builder = new StringBuilder("");
		builder.Append(GetNamespaceString());
		builder.Append(" ").Append(commandInput.GetArgumentTemplate());
		builder.Append("\n").Append(helpText);
		foreach(string argHelpText in commandInput.GetArgumentHelp()) {
			builder.Append("\n\t").Append(argHelpText);
		}
		return builder.ToString();
	}

	public abstract class BTCommandInput
	{
		private List<BTConsoleArgument> requiredArguments = null;
		private List<BTConsoleArgument> optionalArguments = null;

		public BTCommandInput() {
			Prepare();
		}

		protected abstract List<BTConsoleArgument> BuildConsoleArguments();

		public void Prepare() {
			requiredArguments = new List<BTConsoleArgument>();
			optionalArguments = new List<BTConsoleArgument>();
			foreach(BTConsoleArgument argument in BuildConsoleArguments()) {
				if(argument.IsOptional()) {
					optionalArguments.Add(argument);
				} else {
					requiredArguments.Add(argument);
				}
			}
		}

		private List<BTConsoleArgument> GetConsoleArguments() {
			if(requiredArguments == null || optionalArguments == null) {
				Prepare();
			}
			return requiredArguments.Concat(optionalArguments).ToList();
		}

		public void ParseArguments(List<string> stringArguments, int consumedArguments) {
			int argumentCount = stringArguments.Count;
			foreach(BTConsoleArgument argument in GetConsoleArguments()) {
				argument.Reset();
				if(consumedArguments < argumentCount) {
					argument.Consume(stringArguments[consumedArguments]);
					consumedArguments++;
				}
			}
		}

		public int TotalArgumentCount() {
			return requiredArguments.Count + optionalArguments.Count;
		}

		public int RequiredArgumentCount() {
			return requiredArguments.Count;
		}

		public string GetArgumentTemplate() {
			List<string> argumentTexts = GetConsoleArguments()
				.Select(arg => arg.GetFormattedDisplayName())
				.ToList();
			return String.Join(" ", argumentTexts);
		}

		public List<string> GetArgumentHelp() {
			return GetConsoleArguments()
				.Select(arg => arg.GetFormattedHelpText())
				.ToList();
		}
	}

	public class BTConsoleArgument
	{
		private string displayName;
		private bool optional;
		private string helpText;
		private Action<object, bool> valueConsumer;
		private Type valueType;

		public BTConsoleArgument(string displayName, bool optional, string helpText, Action<object, bool> valueConsumer, Type valueType) {
			this.displayName = displayName;
			this.optional = optional;
			this.helpText = helpText;
			this.valueConsumer = valueConsumer;
			this.valueType = valueType;
		}

		public bool IsOptional() {
			return optional;
		}

		public void Reset() {
			valueConsumer.Invoke(valueType.IsValueType ? Activator.CreateInstance(valueType) : null, false);
		}

		public void Consume(string value) {
			if(valueType.IsEnum) {
				valueConsumer.Invoke(Enum.Parse(valueType, value, true), true);
			} else {
				valueConsumer.Invoke(Convert.ChangeType(value, valueType, CultureInfo.InvariantCulture), true);
			}
		}

		public string GetFormattedDisplayName() {
			if(optional) {
				return "<" + displayName + ">";
			} else {
				return "[" + displayName + "]";
			}
		}

		public string GetFormattedHelpText() {
			return GetFormattedDisplayName()
				+ ": "
				+ valueType.Name
				+ " - "
				+ helpText;
		}
	}
}