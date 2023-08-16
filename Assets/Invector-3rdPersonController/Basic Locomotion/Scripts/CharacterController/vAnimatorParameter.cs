using UnityEngine;
namespace Invector
{
    /// <summary>
    /// This class is useful when you don't make sure if parameter of the Animator exist
    /// </summary>
    public class vAnimatorParameter
    {
        readonly AnimatorControllerParameter _parameter;
  
        public static implicit operator int(vAnimatorParameter a)
        {
            if (a.isValid) return a._parameter.nameHash;
            else
                return -1;
        }

        public readonly bool isValid;

        public vAnimatorParameter(Animator animator, string parameter)
        {
            if (animator && animator.ContainsParameter(parameter))
            {
                _parameter = animator.GetValidParameter(parameter);
                this.isValid = true;
            }

            else this.isValid = false;
        }
       
    }

  
    /// <summary>
    /// Extencion class for Animator Paramentes
    /// </summary>
    public static class vAnimatorParameterHelper
    {
        /// <summary>
        /// Get Animator paramenter
        /// </summary>
        /// <param name="animator">Target animator</param>
        /// <param name="paramenterName">Target animator paramenter</param>
        /// <returns></returns>
        public static AnimatorControllerParameter GetValidParameter(this Animator animator, string paramenterName)
        {
            if (null == animator)
            {
                return null;
            }
            return System.Array.Find(animator.parameters, p => p.name.Equals(paramenterName));
        }
        public static bool GetValidParameter(this Animator animator,string paramenterName, out AnimatorControllerParameter parameter)
        {
            parameter = animator.GetValidParameter(paramenterName);
            return parameter != null;
        }

        /// <summary>
        /// Check if Animator has specific paramenter
        /// </summary>
        /// <param name="animator">Target animator</param>
        /// <param name="paramenterName">Target animator paramenter</param>
        /// <returns></returns>
        public static bool ContainsParameter(this Animator animator, string paramenterName)
        {
            if (null == animator)
            {
                return false;
            }
            return System.Array.Exists(animator.parameters,p=>p.name.Equals(paramenterName));
        }
        /// <summary>
        /// Check if Animator has specific paramenter
        /// </summary>
        /// <param name="animator">Target animator</param>
        /// <param name="parameterName">Target animator paramenter</param>
        /// <param name="parameterType">Target animator paramenter type</param>
        /// <returns></returns>
        public static bool ContainsParameter(this Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
        {
            if (null == animator)
            {
                return false;
            }
            return System.Array.Exists(animator.parameters, p => p.name.Equals(parameterName) && p.type.Equals(parameterType)); ;
        }
    }
}