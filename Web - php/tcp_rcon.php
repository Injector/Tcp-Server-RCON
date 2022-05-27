<?php
$address = "0.0.0.0";
$port = 27015;
$rcon = "RconPass1234";
$cmdId = "1";
$cmdArguments = ""; // formation: arg1|arg2|arg3

$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
if ($socket === false)
{
	echo "Socket creation failed: " . socket_strerror(socket_last_error()) . "\n";
	return;
}
echo "Connecting...\n";

$result = socket_connect($socket, $address, $port);
if ($result === false)
{
	echo "Socket connection failed: $result " . socket_strerror(socket_last_error($socket)) . "\n";
	return;
}
else
{
	echo "Connected\n";
}

$in = strlen($cmdArguments) > 0 ? "$rcon|$cmdId|$cmdArguments" : "$rcon|$cmdId";
$out = "";

echo "Sending cmd...\n";
socket_write($socket, $in, strlen($in));
echo "Sent cmd\n";
echo "Reading response\n";

while ($out = socket_read($socket, 2048))
{
	echo $out;
}
socket_close($socket);
?>