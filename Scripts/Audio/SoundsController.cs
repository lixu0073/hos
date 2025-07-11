using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;

/// <summary>
/// 音效控制器，全局单例模式管理游戏中所有的音效和背景音乐播放，包括音效开关、音量控制等功能
/// </summary>
public class SoundsController : MonoBehaviour
{
    public static SoundsController Instance;

    public AudioSource musicSource;
    public List<AudioSource> Sounds = new List<AudioSource>();
    public List<AudioSource> EnviroSources;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CheckSoundSettings()
    {
        if (IsMusicEnabled())
        {
            UnmuteMusic();
        }
        else
        {
            MuteMusic();
        }

        if (IsSoundEnabled())
        {
            UnmuteSound();
        }
        else
        {
            MuteSound();
        }
    }

    public bool IsMusicEnabled()
    {
        //0 = enabled, 1 = disabled
        return PlayerPrefs.GetInt("music_muted") == 0;
    }

    public bool IsSoundEnabled()
    {
        //0 = enabled, 1 = disabled
        return PlayerPrefs.GetInt("sound_muted") == 0;
    }

    public void MuteMusic()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(MuteMusicOnTheMainThread());
    }

    public IEnumerator MuteMusicOnTheMainThread()
    {
        if (this == null || musicSource == null)
        {
            yield return null;
        }
        musicSource.enabled = false;
        PlayerPrefs.SetInt("music_muted", 1);
        yield return null;
    }

    public void PauseSoundsAndMusic()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(PauseSoundAndMusicOnTheMainThread());
    }

    public IEnumerator PauseSoundAndMusicOnTheMainThread()
    {
        musicSource.enabled = false;
        int soundSourcesCount = Sounds.Count;
        for (int i = 0; i < soundSourcesCount; ++i)
        {
            Sounds[i].enabled = false;
        }

        int enviroSourcesCount = EnviroSources.Count;
        for (int i = 0; i < enviroSourcesCount; ++i)
        {
            EnviroSources[i].enabled = false;
        }
        yield return null;
    }

    public void UnmuteMusic()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(UnmuteMusicOnTheMainThread());
    }

    public IEnumerator UnmuteMusicOnTheMainThread()
    {
        if (this != null && musicSource != null)
        {
            musicSource.enabled = true;
            musicSource.Play();
        }
        PlayerPrefs.SetInt("music_muted", 0);
        yield return null;
    }

    public void PlayMusic()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(PlayMusicOnTheMainThread());
        musicSource.Play();
    }

    public IEnumerator PlayMusicOnTheMainThread()
    {
        musicSource.Play();
        yield return null;
    }

    public void MuteSound()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(MuteSoundOnTheMainThread());
    }

    public IEnumerator MuteSoundOnTheMainThread()
    {
        PlayerPrefs.SetInt("sound_muted", 1);

        if (this == null || Sounds == null || EnviroSources == null)
        {
            yield return null;
        }

        int soundSourcesCount = Sounds.Count;
        for (int i = 0; i < soundSourcesCount; ++i)
        {
            Sounds[i].enabled = false;
        }

        int enviroSourcesCount = EnviroSources.Count;
        for (int i = 0; i < enviroSourcesCount; ++i)
        {
            EnviroSources[i].enabled = false;
        }
        yield return null;
    }

    public void UnmuteSound()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(UnmuteSoundOnTheMainThread());
    }

    public IEnumerator UnmuteSoundOnTheMainThread()
    {
        PlayerPrefs.SetInt("sound_muted", 0);

        if (this == null || Sounds == null || EnviroSources == null)
        {
            yield return null;
        }

        int soundSourcesCount = Sounds.Count;
        for (int i = 0; i < soundSourcesCount; ++i)
        {
            Sounds[i].enabled = true;
        }

        int enviroSourcesCount = EnviroSources.Count;
        for (int i = 0; i < enviroSourcesCount; ++i)
        {
            EnviroSources[i].enabled = true;
        }
        yield return null;
    }

    public void PlaySoundOnSelected(RotatableObject rObj)
    {
        if (rObj.state == RotatableObject.State.building)
        {
            PlayConstruction();
        }
        else if (rObj.state == RotatableObject.State.waitingForUser)
        {
            PlayCheering();
        }
        else
        {
            switch (rObj.Tag)
            {
                case "BalmLab":
                    PlayBalmLabSelect();
                    break;
                case "CapsuleLab":
                    PlayCapsuleLabSelect();
                    break;
                case "DripsLab":
                    PlayDripsLabSelect();
                    break;
                case "ExtractLab":
                    PlayExtractLabSelect();
                    break;
                case "EyeDropsLab":
                    PlayEyeDropsLabSelect();
                    break;
                case "FizzyTabLab":
                    PlayFizzyTabLabSelect();
                    break;
                case "Inhaler Maker":
                    PlayInhaleMistsLabSelect();
                    break;
                case "JellyLab":
                    PlayJellyLabSelect();
                    break;
                case "NoseLab":
                    PlayNoseDropsLabSelect();
                    break;
                case "PillLab":
                    PlayPillLabSelect();
                    break;
                case "ShotLab":
                    PlayShotLabSelect();
                    break;
                case "SyrupLab":
                    PlaySyrupLabSelect();
                    break;
                case "ElixirLab":
                    PlayElixirLab();
                    break;
                case "EliStore":
                    PlayElixirSelect();
                    break;
                case "EliTank":
                    PlayElixirSelect();
                    break;
                case "MicroscopeLab":
                    PlayMicroscopeLab();
                    break;
                case "BloodTest":
                    PlayBloodLab();
                    break;
                default:
                    PlayButtonClick2();
                    break;
            }
        }
    }

    public void StartAmbulancev01() { PlaySound(0); }
    public void StopAmbulancev01() { StopSound(0); }

    public void PlayBalmLabSelect() { PlaySound(1); } //done

    public void PlayButtonClick(bool isInitalized)
    {
        if (!isInitalized)
        {
            PlaySound(2);
        }
    } //done

    public void PlayButtonClickInactive(bool isInitalized)
    {
        if (!isInitalized)
        {
            PlaySound(45);
        }
    }

    public void PlayAnySound(int index) { PlaySound(index); }
    public void StopAnySound(int index) { StopSound(index); }
    public void PlayCapsuleLabSelect() { PlaySound(3); } //done            
    public void PlayChildCured() { PlaySound(4); }
    public void PlayChildCured2() { PlaySound(5); }
    public void PlayCoinIncrease() { PlaySound(6); } //done
    public void PlayCollectElixir() { PlaySound(7); } //done
    public void PlaySeedElixir() { PlaySound(7); } //done
    public void PlayDecoSelect() { PlaySound(8); }
    public void PlayDripsLabSelect() { PlaySound(9); } //done
    public void PlayElixirSelect() { PlaySound(10); } //done
    public void PlayExtractLabSelect() { PlaySound(11); } //done
    public void PlayEyeDropsLabSelect() { PlaySound(12); } //done
    public void PlayFemaleAdultCured() { PlaySound(13); } //done
    public void PlayFemaleDoctorRoomSelect() { PlaySound(14); }
    public void PlayFizzyTabLabSelect() { PlaySound(15); } //done
    public void PlayInhaleMistsLabSelect() { PlaySound(16); } //done
    public void PlayJellyLabSelect() { PlaySound(17); } //done                      
    public void PlayMaleAdultCured() { PlaySound(18); } //done                           
    public void PlayMaleDoctorRoomSelect() { PlaySound(19); }
    public void PlayNoseDropsLabSelect() { PlaySound(20); } //done                     
    public void PlayObjectUpgrade() { PlaySound(21); } //done tylko elixir storage, panacea storage i panacea collector
    public void PlayPatientCardOpen() { PlaySound(22); } //done                         
    public void PlayPillLabSelect() { PlaySound(23); } //done                           
    public void PlayPlantSelectv04() { PlaySound(24); }
    public void PlayPositiveEnergyIncrease() { PlaySound(25); }
    public void PlayProbeTableSelect() { PlaySound(26); } //done                        
    public void PlayShotLabSelect() { PlaySound(27); } //done                       
    public void PlayStorageIncrease() { PlaySound(28); }
    public void PlaySyrupLabSelect() { PlaySound(29); } //done                        
    public void PlayTubeHolderSelect() { PlaySound(30); } //done chwilowo na panacea storage
    public void PlayXPIncrease() { PlaySound(31); }
    public void PlayBirds() { PlaySound(32); }
    public void PlayBugs() { PlaySound(33); }
    public void PlayCashing() { PlaySound(34); } //done - 
    public void PlayConstruction() { PlaySound(35); }
    public void PlayCough() { PlaySound(36); } //done
    public void PlayDiamondSpend() { PlaySound(37); } // done
    public void PlayDoctorsRoomCure() { PlaySound(38); }
    public void PlayEmmaHint() { PlaySound(39); } //done
    public void PlayRustle() { PlaySound(40); } //done
    public void PlayTreatmentRoomCure() { PlaySound(41); } //done
    public void PlayWater() { PlaySound(42); }
    public void PlayWind() { PlaySound(43); }
    public void PlayButtonClick2() { PlaySound(44); } //obiekty inne niż interface, done (to be tested)
    public void PlayCureInSlot() { PlaySound(45); } //done
    public void PlayBubblePop() { PlaySound(46); } //done
    public void PlayPopUp() { PlaySound(47); }  //done
    public void PlayPrizeCollect() { PlaySound(48); }
    public void PlayBoxOpen() { PlaySound(49); } //done
    public void PlayHit() { PlaySound(50); } //done
    public void PlayReward() { PlaySound(51); } //done
    public void PlayChooperIn() { PlaySound(52); } //done
    public void PlayChooperOut() { PlaySound(53); } //done
    public void PlayDeliveryCarIn() { PlaySound(54); } //done
    public void PlayDeliveryCarOut() { PlaySound(55); } //done
    public void StopDeliverySound() { StopSound(55); } //done
    public void PlayPanaceaBubble() { PlaySound(56); } //done
    public void PlayCheering() { PlaySound(57); } // done
    public void PlayMagicPoof() { PlaySound(58); }
    public void PlayPoof() { PlaySound(59); }
    public void PlayAlert() { PlaySound(60); }
    public void PlayInfoButton() { PlaySound(61); }
    public void PlayLvlUp() { PlaySound(62); }
    public void PlayFBCarHorn() { PlaySound(63); }
    public void PlayFBEngine() { PlaySound(64); }
    public void PlayChooperWait() { PlaySound(65); }
    public void StopHeliWait() { StopSound(65); }
    public void PlayFlyOutEpidemy() { PlaySound(66); }
    public void PlayFlyTeaser() { PlaySound(67); }
    public void StopFlyTeaser() { StopSound(67); }
    public void PlayBubbleLottery() { PlaySound(68); }
    public void PlayShortBubble() { PlaySound(69); }
    public void PlayBubbleDie() { PlaySound(70); }
    public void PlayClockTicking() { PlaySound(74); }
    public void PlayZip() { PlaySound(75); }
    public void PlayMagicPoof2() { PlaySound(76); }
    public void PlayFlyingStar() { PlaySound(77); }
    public void PlaySeeding() { PlaySound(78); }
    public void PlayShoveling() { PlaySound(79); }
    public void PlayFaxMachine() { PlaySound(80); }
    public void PlayMicroscopeLab() { PlaySound(81); }
    public void PlayEventChest() { PlaySound(82); }
    public void PlayEventPopUp() { PlaySound(83); }
    public void PlayMicroscopeTutorial1() { PlaySound(84); }
    public void PlayMicroscopeTutorial2() { PlaySound(85); }
    public void PlayBacteria() { PlaySound(86); }
    public void PlayDecontamination() { PlaySound(87); }
    public void PlayPatientContaminated() { PlaySound(88); }
    public void PlayPatientDecontaminated() { PlaySound(89); }
    public void PlayDeer() { PlaySound(90); }
    public void PlayEpidemyAmbient() { PlaySound(91); }
    public void PlayFire() { PlaySound(92); }
    public void PlayKeyboard() { PlaySound(93); }
    public void PlayBird() { PlaySound(94); }
    public void PlayElixirLab() { PlaySound(95); }
    public void PlayFireLoop() { PlaySound(96); }
    public void PlayChampagne() { PlaySound(97); }
    public void PlayFriendsGift() { PlaySound(98); }
    public void PlayPumpkinSmash() { PlaySound(99); }
    public void PlayPatientHelpRequest() { PlaySound(100); }
    public void PlayCollectVitamin() { PlaySound(101); }
    public void BabyBorn() { PlaySound(102); }
    public void HBGiftClaim() { PlaySound(103); }
    public void PlayLaborCover() { PlaySound(104); }
    public void PlayBloodLab() { PlaySound(105); }
    public void PlayCarChirp() { PlaySound(106); }
    public void PlayCarAlarm() { PlaySound(107); }

    private void PlaySound(int index)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(PlaySoundOnTheMainThread(index));
    }

    public IEnumerator PlaySoundOnTheMainThread(int index)
    {
        if (!IsSoundEnabled())
        {
            yield return null;
        }

        if (Sounds[index].isActiveAndEnabled)
            Sounds[index].Play();
        yield return null;
    }

    private void StopSound(int index)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(StopSoundOnTheMainThread(index));
    }

    public IEnumerator StopSoundOnTheMainThread(int index)
    {
        if (Sounds[index].isPlaying)
        {
            Sounds[index].Stop();
        }
        yield return null;
    }

    public void StopAllSounds()
    {
        int soundSourcesCount = Sounds.Count;
        for (int i = 0; i < soundSourcesCount; ++i)
        {
            StopSound(i);
        }
    }
}
