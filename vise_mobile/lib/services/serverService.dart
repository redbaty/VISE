import 'package:signalr_client/signalr_client.dart';
import 'package:vise_mobile/exceptions/notImplemented.dart';
import 'package:vise_mobile/services/playerService.dart';

class ServerService {
  PlayerService getPlayerService() {
    throw new NotImplementedException();
  }

  Future<HubConnection> buildSignalRConnection() async {
    final hubConnection = HubConnectionBuilder().withUrl('$baseUrl/hubs/phone').build();
    await hubConnection.start();
    return hubConnection;
  }
}
