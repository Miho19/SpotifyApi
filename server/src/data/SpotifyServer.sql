DROP DATABASE IF EXISTS SpotifyServer;
CREATE DATABASE SpotifyServer;
use SpotifyServer;

DROP TABLE IF EXISTS spotifyUserData;
DROP TABLE IF EXISTS users;

CREATE TABLE users (
    u_id INTEGER PRIMARY KEY AUTO_INCREMENT,
    u_sid VARCHAR(255) NOT NULL
);

CREATE TABLE spotifyUserData (
    spotify_id VARCHAR(255) PRIMARY KEY NOT NULL, 
    u_id INTEGER NOT NULL, 
    displayName VARCHAR(255) NOT NULL, 
    accessToken VARCHAR(255) NOT NULL, 
    imageURL VARCHAR(255) NOT NULL, 
    FOREIGN KEY (u_id) REFERENCES users(u_id) ON DELETE CASCADE
);

INSERT INTO users (u_sid) VALUES("fake user session id");
INSERT INTO spotifyUserData (spotify_id, u_id, displayName, accessToken, imageURL) VALUES("oauth2|spotify|spotify:user:1253470477", 1, "Josh April", "access token", "image url");

SELECT * FROM spotifyUserData;
