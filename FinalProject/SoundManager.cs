using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace FinalProject
{
    public static class SoundManager
    {
        private static SoundEffect _hitSound, _swordSwing, _walkSound, _jumpSound, _landSound, _deathSound;
        private static Song _bgMusic;

        public static void LoadContent(ContentManager content)
        {
            _hitSound = content.Load<SoundEffect>("hit"); // hit.wav
            _swordSwing = content.Load<SoundEffect>("swordSwing"); //swordSwing.wav
            _walkSound = content.Load<SoundEffect>("walk"); // walk.wav
            _jumpSound = content.Load<SoundEffect>("jump"); // jump.wav
            _landSound = content.Load<SoundEffect>("land"); // land.wav
            _deathSound = content.Load<SoundEffect>("death"); // death.wav
            _bgMusic = content.Load<Song>("bgmusic"); // bgmusic.mp3
        }

        public static void PlayHitSound()
        {
            _hitSound.Play();
        }

        public static void PlaySwordSwing()
        {
            Debug.WriteLine("Swing SFX");
            _swordSwing.Play();
        }

        public static void PlayWalkSound( )
        {
            _walkSound.Play( );
        }

        public static void PlayJumpSound( )
        {
            _jumpSound.Play( );
        }

        public static void PlayLandSound( )
        {
            _landSound.Play( );
        }

        public static void PlayDeathSound( )
        {
            _deathSound.Play( );
        }

        public static void PlayBackgroundMusic()
        {
            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(_bgMusic);
            }
        }

        public static void StopBackgroundMusic()
        {
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Stop();
        }
    }
}
