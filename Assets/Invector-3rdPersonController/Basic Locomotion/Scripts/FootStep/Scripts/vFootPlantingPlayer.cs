using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector
{
    [System.Serializable]
    public abstract class vFootStepBase : MonoBehaviour
    {
        // The different surfaces and their sounds.
        public vAudioSurface defaultSurface;
        public List<vAudioSurface> customSurfaces;

        /// <summary>
        /// Play a foot step effect passing the <seealso cref="FootStepObject"/> to determine what surface is stepping
        /// </summary>
        /// <param name="footStepObject">Foot Step object with surface information</param>
        /// <param name="spawnParticle">Spawn Particle ?</param>
        /// <param name="spawnStepMark">Spwan Step Mark ?</param>
        /// <param name="volume">Audio effect volume</param>
        public virtual void SpawnSurfaceEffect(FootStepObject footStepObject)
        {
            if(footStepObject!=null)
                for (int i = 0; i < customSurfaces.Count; i++)
                    if (customSurfaces[i] != null && ContainsTexture(footStepObject.name, customSurfaces[i]))
                    {
                        customSurfaces[i].SpawnSurfaceEffect(footStepObject);
                        return;
                    }
            if (defaultSurface != null)
            {
                defaultSurface.SpawnSurfaceEffect(footStepObject);
            }
        }

        /// <summary>
        /// Ccheck if AudioSurface Contains texture in TextureName List
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surface"></param>
        /// <returns></returns>
        protected virtual bool ContainsTexture(string name, vAudioSurface surface)
        {
            for (int i = 0; i < surface.TextureOrMaterialNames.Count; i++)
                if (name.Contains(surface.TextureOrMaterialNames[i]))
                    return true;

            return false;
        }
        /// <summary>
        /// Step on Terrain
        /// </summary>
        /// <param name="footStepObject"></param>
        public abstract void StepOnTerrain(FootStepObject footStepObject);
        /// <summary>
        /// Step on Mesh
        /// </summary>
        /// <param name="footStepObject"></param>
        public abstract void StepOnMesh(FootStepObject footStepObject);
        /// <summary>
        /// Play foot Step sound
        /// </summary>
        public abstract void PlayFootStepEffect();
        /// <summary>
        /// Play Foot Step Effect directly using animation Event
        /// </summary>
        /// <param name="evt"></param>
        public virtual void PlayFootStep(AnimationEvent evt) { }      
        /// <summary>
        /// Play Left Foot Step Effect directly using animation Event
        /// </summary>
        /// <param name="evt"></param>
        public virtual void PlayFootStepLeft(AnimationEvent evt) { }
        /// <summary>
        /// Play Right Foot Step Effect directly using animation Event
        /// </summary>
        /// <param name="evt"></param>
        public virtual void PlayFootStepRight(AnimationEvent evt) { }

    }
}