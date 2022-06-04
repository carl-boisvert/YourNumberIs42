using System.Collections.Generic;
using UnityEngine;

public enum PlayerAudioType {
    Interacting,
    Sprinting,
    Walking
}
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _interactingClips = null;
    [SerializeField] private List<AudioClip> _walkingClips = null;
    [SerializeField] private List<AudioClip> _sprintingClips = null;

    private float _nextFootstepTimer = 0f;

    public void PlayPlayerSFX(PlayerAudioType type) {
        if (_nextFootstepTimer > Time.time) {
            return;
        }

        AudioClip clip = null;

        switch (type) {
            case PlayerAudioType.Interacting:
                clip = _interactingClips[Random.Range(0, _interactingClips.Count)];
                break;
            case PlayerAudioType.Sprinting:
                clip = _sprintingClips[Random.Range(0, _sprintingClips.Count)];
                //_nextFootstepTimer = Time.time + (clip.length / 4f;
                _nextFootstepTimer = Time.time + clip.length;
                break;
            case PlayerAudioType.Walking:
                clip = _walkingClips[Random.Range(0, _walkingClips.Count)];
                _nextFootstepTimer = Time.time + clip.length;
                break;
            default:
                break;
        }

        if (clip == null) {
            Debug.LogError("AudioClip is null");
            return;
        }

        if (type == PlayerAudioType.Interacting) {
            AudioManager.Instance.PlayPlayerEffect(clip, AudioManager.AudioType.SFX);
        }
        else if (type == PlayerAudioType.Sprinting || type == PlayerAudioType.Walking){
            AudioManager.Instance.PlayPlayerEffect(clip, AudioManager.AudioType.Footsteps);
        }

    }
}
