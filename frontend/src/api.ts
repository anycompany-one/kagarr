import { GameResource, WishlistResource, DealResource } from './types';

const API_BASE = '/api/v1';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: {
      'Content-Type': 'application/json',
    },
    ...options,
  });

  if (!response.ok) {
    throw new Error(`API error: ${response.status} ${response.statusText}`);
  }

  return response.json();
}

export function getGames(): Promise<GameResource[]> {
  return request<GameResource[]>('/game');
}

export function getGame(id: number): Promise<GameResource> {
  return request<GameResource>(`/game/${id}`);
}

export function addGame(game: Partial<GameResource>): Promise<GameResource> {
  return request<GameResource>('/game', {
    method: 'POST',
    body: JSON.stringify(game),
  });
}

export function updateGame(id: number, game: Partial<GameResource>): Promise<GameResource> {
  return request<GameResource>(`/game/${id}`, {
    method: 'PUT',
    body: JSON.stringify(game),
  });
}

export function deleteGame(id: number): Promise<void> {
  return request<void>(`/game/${id}`, { method: 'DELETE' });
}

export function searchGames(term: string): Promise<GameResource[]> {
  return request<GameResource[]>(`/game/lookup?term=${encodeURIComponent(term)}`);
}

export function lookupGame(igdbId: number): Promise<GameResource> {
  return request<GameResource>(`/game/lookup/${igdbId}`);
}

// Wishlist
export function getWishlist(): Promise<WishlistResource[]> {
  return request<WishlistResource[]>('/wishlist');
}

export function getWishlistItem(id: number): Promise<WishlistResource> {
  return request<WishlistResource>(`/wishlist/${id}`);
}

export function addToWishlist(item: Partial<WishlistResource>): Promise<WishlistResource> {
  return request<WishlistResource>('/wishlist', {
    method: 'POST',
    body: JSON.stringify(item),
  });
}

export function updateWishlistItem(
  id: number,
  item: Partial<WishlistResource>,
): Promise<WishlistResource> {
  return request<WishlistResource>(`/wishlist/${id}`, {
    method: 'PUT',
    body: JSON.stringify(item),
  });
}

export function removeFromWishlist(id: number): Promise<void> {
  return request<void>(`/wishlist/${id}`, { method: 'DELETE' });
}

// Deals
export function getDeals(wishlistItemId: number): Promise<DealResource> {
  return request<DealResource>(`/deal/${wishlistItemId}`);
}

export function checkDeals(wishlistItemId: number): Promise<DealResource> {
  return request<DealResource>(`/deal/${wishlistItemId}/check`, { method: 'POST' });
}

export function checkAllDeals(): Promise<DealResource[]> {
  return request<DealResource[]>('/deal/check', { method: 'POST' });
}
