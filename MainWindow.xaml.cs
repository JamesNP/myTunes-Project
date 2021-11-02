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
        private Point startPoint;

        public bool songPlaying { get; set; }
        public string dropPlaylist { get; set; }
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
                headerRemove.Header = "Remove";
            }
            else
            {
                DataTable allPlaylistSongs = musicRepo.SongsForPlaylist(selectedPlaylist);
                songsDataGrid.ItemsSource = allPlaylistSongs.DefaultView;
                headerRemove.Header = "Remove from Playlist";
            }
            
        }

        private void PlayCommand_Executed(object sender, RoutedEventArgs e)
        {
            //Gets data from selected row to play song
            var temp = songsDataGrid.SelectedItem as DataRowView;
            int songId = Convert.ToInt32(temp.Row.ItemArray[0]);
            Song s = musicRepo.GetSong(songId);

            mediaPlayer.Open(new Uri(s.Filename));
            mediaPlayer.Play();
            songPlaying = true;
        }

        private void PlayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (songsDataGrid.SelectedItems.Count > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            songPlaying = false;
            mediaPlayer.Stop();
        }

        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = songPlaying;
        }

        private void playMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var temp = songsDataGrid.SelectedItem as DataRowView;
            int songId = Convert.ToInt32(temp.Row.ItemArray[0]);
            Song s = musicRepo.GetSong(songId);

            mediaPlayer.Open(new Uri(s.Filename));
            mediaPlayer.Play();
            songPlaying = true;
        }

        private void removeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var temp = songsDataGrid.SelectedItem as DataRowView;
            int songId = Convert.ToInt32(temp.Row.ItemArray[0]);
            Song s = musicRepo.GetSong(songId);
            string selectedPlaylist = playlistListBox.SelectedItem as string;
            int position = musicRepo.GetLastPosition(selectedPlaylist);

            if (selectedPlaylist != "All Music")
            {
                musicRepo.RemoveSongFromPlaylist(position,
                        songId, selectedPlaylist);
            }
            else
            {
                musicRepo.DeleteSong(songId);
            }


        }

        private void aboutToolButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Show();
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
            else if (playlistName != null)
            {
                MessageBox.Show("Playlist already exists, please try a different name.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void songsDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position on left button click
            startPoint = e.GetPosition(null);
        }

        private void songsDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

    

            // Start the drag-drop if mouse has moved far enough
            if (e.LeftButton == MouseButtonState.Pressed &&
                songsDataGrid.SelectedItems.Count > 0 &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DataRowView rowView = songsDataGrid.SelectedItem as DataRowView;
                int songId = Convert.ToInt32(rowView.Row.ItemArray[0]);
                string textSongId = songId.ToString();
                Console.WriteLine("Dragging song " + textSongId);

                // Initiate dragging the text from the textbox
                DragDrop.DoDragDrop(songsDataGrid, textSongId, DragDropEffects.Copy);
            }
        }

        private void playlistListBox_DragOver(object sender, DragEventArgs e)
        {
            //By default, no drag and drop
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //Reads in song ID as an integer
                int textSongId = Convert.ToInt32((string)e.Data.GetData(DataFormats.StringFormat));

                Label playlist = sender as Label;
                if (playlist != null)
                {
                    if (playlist.AllowDrop)
                    {
                        dropPlaylist = (string)playlist.Content;
                        e.Effects = DragDropEffects.Copy;
                    }
                }
            }
        }

        private void playlistListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //Reads in song ID as an integer
                int songId = Convert.ToInt32((string)e.Data.GetData(DataFormats.StringFormat));

                //Add song to playlist
                musicRepo.AddSongToPlaylist(songId, dropPlaylist);
            }
        }
    }
}
