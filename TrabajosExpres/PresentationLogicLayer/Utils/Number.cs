
namespace TrabajosExpres.PresentationLogicLayer.Utils
{
    /// <summary>
    /// This is an enumeration of numbers
    /// </summary>
    public enum NumberValues
    {
        /// <summary>
        /// Number Zero
        /// </summary>
        ZERO = 0,

        /// <summary>
        /// Number One
        /// </summary>
        ONE = 1,

        /// <summary>
        /// Number Two
        /// </summary>
        TWO = 2,

        /// <summary>
        /// Number Three
        /// </summary>
        THREE = 3,

        /// <summary>
        /// Number Four
        /// </summary>
        FOUR = 4,

        /// <summary>
        /// Number Five
        /// </summary>
        FIVE = 5,

        /// <summary>
        /// Number Ten
        /// </summary>
        TEN = 10,

        /// <summary>
        /// Number Eighteen
        /// </summary>
        EIGHTEEN = 18
    }

    /// <summary>
    /// This class is for numbers
    /// </summary>
    public static class Number
    {

        /// <summary>
        /// Method that returns the number of the enumeration
        /// </summary>
        /// <param name="numberValues">Number value</param>
        /// <returns>Equivalent enumeration number</returns>
        public static int NumberValue(NumberValues numberValues)
        {
            int number = (int)numberValues;
            return number;
        }
    }
}
