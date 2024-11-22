using UnityEngine;

namespace UCT.Global.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public AudioSource audioSource;
        private float _clock;
        private bool _isPlay;

        private void Update()
        {
            if (_isPlay)
            {
                _clock += Time.deltaTime;
                if (_clock >= audioSource.clip.length) AudioController.Instance.ReturnPool(gameObject);
            }
        }

        private void OnEnable()
        {
            _isPlay = false;
            _clock = 0;
        }

        public void Playing(AudioClip clip)
        {
            if (!_isPlay)
            {
                audioSource.clip = clip;
                audioSource.Play();
                _isPlay = true;
            }
        }
    }
}