import 'package:flutter/material.dart';
import 'package:signalr_client/signalr_client.dart';
import 'package:vise_mobile/models/playerMedia.dart';
import 'package:vise_mobile/services/playerService.dart';
import 'package:vise_mobile/services/serverService.dart';

class PlayerStatusControl extends StatefulWidget {
  PlayerStatusControl({Key key}) : super(key: key);

  _PlayerStatusControlState createState() => _PlayerStatusControlState();
}

class _PlayerStatusControlState extends State<PlayerStatusControl> {
  PlayerMedia _currentMedia;
  HubConnection _hubConnection;
  bool _isConnected;

  @override
  void initState() {
    super.initState();
    final playlistService = PlaylistService();
    final serverService = ServerService();
    _isConnected = false;

    playlistService.getCurrentMedia().then((currentMedia) {
      setState(() {
        _currentMedia = currentMedia;
      });
    });

    serverService.buildSignalRConnection().then((hubConnection) {
      _hubConnection = hubConnection;
      _hubConnection.onclose((v) {
        setState(() {
          _isConnected = false;
        });
      });

      setState(() {
        _isConnected = true;
      });

      hubConnection.on('playlist_update', (newMedia) {
        setState(() {
          _currentMedia = PlayerMedia.fromJson(newMedia[0]);
        });
      });
    }, onError: (e) {
      setState(() {
        _isConnected = false;
      });
    });
  }

  @override
  void dispose() {
    _hubConnection.stop();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return _isConnected ? Column(
      children: <Widget>[
        Text(_currentMedia != null
            ? 'Current song: ' + _currentMedia.title
            : 'No song playing')
      ],
    ) : Column(children: <Widget>[Text('Not conneted')],);
  }
}
