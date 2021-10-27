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

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void playMenuItem_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void removeMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void aboutToolButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void songToolButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void playlistToolButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
