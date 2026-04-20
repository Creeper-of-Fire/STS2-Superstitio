using Superstitio.Api.Utils;

namespace Superstitio.Api.CustomKeywords;

/// <summary>
/// 
/// </summary>
public record SpttWord(string? Name = null) : CustomWord(SuperstitioLocStringFactory.Instance, Name);