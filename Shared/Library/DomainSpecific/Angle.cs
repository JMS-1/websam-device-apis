using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace SharedLibrary.DomainSpecific;



/// <summary>
/// Angle (°) as domain specific number.
/// </summary>
public readonly struct Angle(double value) : IDomainSpecificNumber
{
    /// <summary>
    /// Create Angle 0.
    /// </summary>
    public Angle() : this(0) { }

    /// <summary>
    /// The real value is always represented as a double.
    /// </summary>
    private readonly double _Value = value;

    /// <inheritdoc/>
    [JsonIgnore]
    public readonly string Unit => "°";

    /// <summary>
    /// Only explicit casting out the pure number is allowed.
    /// </summary>
    /// <param name="Angle">Some Angle.</param>
    public static explicit operator double(Angle Angle) => Angle._Value;

    /// <summary>
    /// Add to Angle.
    /// </summary>
    /// <param name="left">One Angle.</param>
    /// <param name="right">Another Angle.</param>
    /// <returns>New Angle instance representing the sum of the parameters.</returns>
    public static Angle operator +(Angle left, Angle right) => new(left._Value + right._Value);

    /// <summary>
    /// Substract from Angle.
    /// </summary>
    /// <param name="left">One Angle.</param>
    /// <param name="right">Another Angle.</param>
    /// <returns>New Angle instance representing the substraction of the parameters.</returns>
    public static Angle operator -(Angle left, Angle right) => new(left._Value - right._Value);

    /// <summary>
    /// Modulo operation on angle.
    /// </summary>
    /// <param name="left">One Angle.</param>
    /// <param name="number">Some number.</param>
    /// <returns>New Angle instance.</returns>
    public static Angle operator %(Angle left, double number) => new(left._Value % number);


    /// <summary>
    /// Division on angle.
    /// </summary>
    /// <param name="left">One Angle.</param>
    /// <param name="number">Some number.</param>
    /// <returns>New Angle instance.</returns>
    public static Angle operator /(Angle left, double number) => new(left._Value / number);

    /// <summary>
    /// Scale Angle by a factor.
    /// </summary>
    /// <param name="angle">Some Angle.</param>
    /// <param name="factor">Factor to apply to the Angle.</param>
    /// <returns>New Angle with scaled value.</returns>
    public static Angle operator *(Angle angle, double factor) => new(angle._Value * factor);

    /// <summary>
    /// Scale Angle by a factor.
    /// </summary>
    /// <param name="angle">Some Angle.</param>
    /// <param name="factor">Factor to apply to the Angle.</param>
    /// <returns>New Angle with scaled value.</returns>
    public static Angle operator *(double factor, Angle angle) => angle * factor;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public double Sin() => Math.Sin(_Value * Math.PI / 180);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string? ToString(string format) => _Value.ToString(format);
}
