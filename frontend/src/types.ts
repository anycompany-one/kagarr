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

export interface WishlistResource {
  id: number;
  title: string;
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
  priceThreshold: number | null;
  notifyOnAnyDeal: boolean;
  autoSearch: boolean;
  added: string;
  currentLowestPrice: number | null;
  currentLowestStore: string | null;
  lastDealCheck: string | null;
}

export interface GameDealResource {
  store: string;
  title: string;
  currentPrice: number;
  regularPrice: number;
  discountPercent: number;
  currencyCode: string;
  dealUrl: string;
  isFree: boolean;
}

export interface DealResource {
  id: number;
  wishlistItemId: number;
  gameTitle: string;
  lowestPrice: number | null;
  lowestPriceStore: string | null;
  lastChecked: string;
  deals: GameDealResource[];
}

export interface ReleaseResource {
  guid: string;
  title: string;
  size: number;
  downloadUrl: string;
  infoUrl: string;
  indexer: string;
  indexerId: number;
  downloadProtocol: string;
  publishDate: string;
  seeders: number;
  leechers: number;
  categories: string[];
}

export interface QueueResource {
  downloadId: string;
  title: string;
  totalSize: number;
  remainingSize: number;
  outputPath: string;
  status: string;
  downloadClient: string;
}

export interface ManualImportRequest {
  path: string;
  gameId: number;
  files?: string[];
}

export interface ImportResultResource {
  success: boolean;
  sourcePath: string;
  destinationPath: string;
  errors: string[];
}

export interface HistoryResource {
  id: number;
  eventType: string;
  gameId: number;
  gameTitle: string;
  sourceTitle: string;
  date: string;
  data: string;
}
