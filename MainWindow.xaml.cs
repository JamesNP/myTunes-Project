using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MusicRepo musicRepo;
        private readonly MediaPlayer mediaPlayer;
        private readonly ObservableCollection<string> playlistNames;
        private readonly System.Data.DataTable songs;
        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            try
            {
                musicRepo = new MusicRepo();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading file: " + e.Message, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Puts all the playlist names into the ListBox
            playlistNames = new ObservableCollection<string>(musicRepo.Playlists);
            playlistNames.Add("All Music");
            playlistNames.Move(playlistNames.Count - 1, 0);
            playlistListBox.ItemsSource = playlistNames;

            songs = musicRepo.Songs;
            songsDataGrid.ItemsSource = songs.DefaultView;
        }

        //Displays songs in selected playlist
        private void playlistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedPlaylist = playlistListBox.SelectedItem as string;

            if (selectedPlaylist == "All Music")
            {
                songsDataGrid.ItemsSource = songs.DefaultView;
            }
            else 
            {
                DataTable allPlaylistSongs = musicRepo.SongsForPlaylist(selectedPlaylist);
                songsDataGrid.ItemsSource = allPlaylistSongs.DefaultView;
            }
            
        }

        private void PlayCommand_Executed(object sender, RoutedEventArgs e)
        {
            //This isn't working
            //How do you get info like song Id out of the data grid?
            Song s = songsDataGrid.SelectedItem as Song;
            mediaPlayer.Open(new Uri(s.Filename));
            mediaPlayer.Play();
        }

        private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void playMenuItem_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void removeMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void aboutToolButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void addSongToolButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                DefaultExt = "*.wma;*.wav;*mp3;*.m4a",
                Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                Song tempSong = musicRepo.AddSong(openFileDialog.FileName);
                songsDataGrid.SelectedItem = tempSong.Id;
            }
        }
        private void addPlaylistToolButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistWindow playlistWindow = new PlaylistWindow();
            playlistWindow.ShowDialog();
            string playlistName = playlistWindow.playlistName;
            if (musicRepo.AddPlaylist(playlistName))
            {
                playlistNames.Add(playlistName);
            }
            else
            {
                MessageBox.Show("Playlist already exists, please try a different name.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        
    }
}
