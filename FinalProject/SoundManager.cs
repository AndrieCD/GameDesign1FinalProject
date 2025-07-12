using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace FinalProject
{
    public static class SoundManager
    {
        private static SoundEffect _hitSound, _swordSwing, _walkSound, _jumpSound, _landSound, _deathSound, _clickSound, _pauseSound, _healSound;
        private static Song _bgMusic, _victoryMusic, _gameOverMusic;

        public static void LoadContent(ContentManager content)
        {
            _hitSound = content.Load<SoundEffect>("hit"); // hit.wav
            _swordSwing = content.Load<SoundEffect>("swordSwing"); //swordSwing.wav
            _walkSound = content.Load<SoundEffect>("walk"); // walk.wav
            _jumpSound = content.Load<SoundEffect>("jump"); // jump.wav
            _landSound = content.Load<SoundEffect>("land"); // land.wav
            _deathSound = content.Load<SoundEffect>("death"); // death.wav
            _clickSound = content.Load<SoundEffect>("click"); // click.wav
            _bgMusic = content.Load<Song>("bgmusic"); // bgmusic.mp3
            _victoryMusic = content.Load<Song>("Victory"); // victory.mp3
            _gameOverMusic = content.Load<Song>("Game Over"); // gameover.mp3
            _pauseSound = content.Load<SoundEffect>("pauseSound"); //pause.wav
            _healSound = content.Load<SoundEffect>("heal");
        }

        public static void PlayHealSound()
        {
            _healSound.Play();
        }

        public static void PlayPauseSound()
        {
            _pauseSound.Play();
        }

        public static void PlayClickSound( )
        {
            _clickSound.Play( );
        }

        public static void PlayHitSound( )
        {
            _hitSound.Play( );
        }

        public static void PlaySwordSwing( )
        {
            _swordSwing.Play( );
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

        public static void PlayBackgroundMusic( )
        {
            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.5f;
                MediaPlayer.Play(_bgMusic);
            }
        }

        public static void StopMusic( )
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop( );
            }
        }

        public static void PlayVictoryMusic( )
        {
            MediaPlayer.Stop( ); 
            MediaPlayer.IsRepeating = false; // set to true if you want it to loop
            MediaPlayer.Volume = 1f;
            MediaPlayer.Play(_victoryMusic);
        }

        public static void PlayGameOverMusic( )
        {
            MediaPlayer.Stop( );
            MediaPlayer.IsRepeating = false; // set to true if you want it to loop
            MediaPlayer.Volume = 1f;
            MediaPlayer.Play(_gameOverMusic);
        }

    }
}
