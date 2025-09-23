{ pkgs }:
{
  deps = [
    pkgs.libsForQt5.index
    pkgs.dotnet-sdk
    pkgs.zip
    pkgs.wget
  ];
}
