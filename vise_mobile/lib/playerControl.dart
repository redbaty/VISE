import 'package:flutter/material.dart';
import 'package:vise_mobile/enums/repeatEnum.dart';
import 'package:vise_mobile/services/playerService.dart';

class PlayerControlWidget extends StatefulWidget {
  PlayerControlWidget({Key key}) : super(key: key);

  _PlayerControlWidgetState createState() => _PlayerControlWidgetState();
}

class _PlayerControlWidgetState extends State<PlayerControlWidget>
    with SingleTickerProviderStateMixin {
  bool isPlaying = true;
  AnimationController _playPauseAnimationController;
  RepeatEnum _currentRepeatState;
  PlayerService _playerService;

  @override
  void dispose() {
    _playPauseAnimationController.dispose();
    super.dispose();
  }

  @override
  void initState() {
    super.initState();
    _playerService = PlayerService();
    _currentRepeatState = RepeatEnum.NoRepeat;
    _playPauseAnimationController =
        AnimationController(duration: Duration(milliseconds: 300), vsync: this);
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: <Widget>[
        Container(
          margin: EdgeInsets.fromLTRB(0, 0, 12, 0),
          child: IconButton(
            color: Colors.white.withOpacity(0.8),
            onPressed: () {
              _playerService.stop();
            },
            icon: Icon(Icons.stop),
          ),
        ),
        Container(
          margin: EdgeInsets.fromLTRB(0, 0, 12, 0),
          child: IconButton(
            color: Colors.white.withOpacity(0.8),
            onPressed: () {
              _playerService.previous();
            },
            icon: Icon(Icons.skip_previous),
          ),
        ),
        FloatingActionButton(
          onPressed: () {
            this.setState(() {
              this.isPlaying = !this.isPlaying;

              if (this.isPlaying) {
                _playPauseAnimationController.forward();
              } else {
                _playPauseAnimationController.reverse();
              }

              if(this.isPlaying){
                _playerService.play();
              } else {
                _playerService.pause();
              }
            });
          },
          child: AnimatedIcon(
              icon: AnimatedIcons.play_pause,
              color: Colors.white,
              progress: _playPauseAnimationController),
          backgroundColor: Colors.red,
        ),
        Container(
          margin: EdgeInsets.fromLTRB(12, 0, 0, 0),
          child: IconButton(
            color: Colors.white.withOpacity(0.8),
            onPressed: () {
              _playerService.next();
            },
            icon: Icon(Icons.skip_next),
          ),
        ),
        Container(
          margin: EdgeInsets.fromLTRB(12, 0, 0, 0),
          child: IconButton(
            color: _currentRepeatState == RepeatEnum.NoRepeat
                ? Colors.white.withOpacity(0.8)
                : Colors.red,
            onPressed: () {
              setState(() {
                if (_currentRepeatState == RepeatEnum.NoRepeat) {
                  _currentRepeatState = RepeatEnum.Repeat;
                } else if (_currentRepeatState == RepeatEnum.Repeat) {
                  _currentRepeatState = RepeatEnum.RepeatOne;
                } else {
                  _currentRepeatState = RepeatEnum.NoRepeat;
                }

                _playerService.toggleRepeat();
              });
            },
            icon: Icon(getRepeatIcon(_currentRepeatState)),
          ),
        ),
      ],
    );
  }

  IconData getRepeatIcon(RepeatEnum currentRepeatState) {
    switch (currentRepeatState) {
      case RepeatEnum.NoRepeat:
        return Icons.repeat;
      case RepeatEnum.Repeat:
        return Icons.repeat;
      case RepeatEnum.RepeatOne:
        return Icons.repeat_one;
    }

    return Icons.repeat;
  }
}
