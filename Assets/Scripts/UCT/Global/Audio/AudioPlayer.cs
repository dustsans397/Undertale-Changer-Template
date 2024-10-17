using UnityEngine;

namespace UCT.Global.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        private bool isPlay;
        public AudioSource audioSource;
        private float clock;

        private void OnEnable()
        {
            isPlay = false;
            clock = 0;
        }

        private void Update()
        {
            if (isPlay)
            {
                clock += Time.deltaTime;
                if (clock >= audioSource.clip.length)
                {
                    AudioController.instance.ReturnPool(gameObject);
                }
            }
        }

        public void Playing(AudioClip clip)
        {
            if (!isPlay)
            {
                audioSource.clip = clip;
                audioSource.Play();
                isPlay = true;
            }
        }
    }
}