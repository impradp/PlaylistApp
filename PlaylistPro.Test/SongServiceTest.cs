using AutoFixture;
using Microsoft.Azure.Cosmos;
using Moq;
using Playlist_Pro.Models;
using Playlist_Pro.Services.Song;
using SongFinder.Models;

namespace PlaylistPro.Test
{
    public class SongServiceTest
    {
        private readonly Fixture _fixture;
        private readonly SongService _songService;
        private readonly Mock<Container> _containerMock;

        public SongServiceTest()
        {
            _fixture = new Fixture();
            _containerMock = new Mock<Container>();
            _songService = new SongService(_containerMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCall_CreateItemAsync_With_CorrectParameters()
        {
            // Arrange
            var song = _fixture.Create<SongModel>();
            var songFinderResponse = _fixture.Create<SongFinderResponse>();

            _containerMock
                .Setup(container => container.CreateItemAsync(
                    It.Is<SongModel>(s =>
                        s.Id == song.Id &&
                        s.Name == song.Name &&
                        s.Platform == song.Platform &&
                        s.Path == song.Path
                    ),
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .Returns(Task.FromResult<ItemResponse<SongModel>?>(null))
                .Verifiable();

            // Act
            await _songService.AddAsync(songFinderResponse,song);

            // Assert
            _containerMock.Verify();
        }
    }
}
