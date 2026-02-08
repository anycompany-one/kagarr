export interface GameResource {
  id: number;
  title: string;
  cleanTitle: string;
  sortTitle: string;
  year: number;
  overview: string;
  igdbId: number;
  steamAppId: number | null;
  platform: string;
  genres: string[];
  developer: string;
  publisher: string;
  releaseDate: string | null;
  images: MediaCover[];
  remoteCover: string;
  path: string;
  monitored: boolean;
  qualityProfileId: number;
  tags: number[];
  added: string;
  rootFolderPath: string;
}

export interface MediaCover {
  coverType: string;
  url: string;
  remoteUrl: string;
}
