import { Routes, Route, Navigate } from 'react-router-dom';
import Navbar from './Components/Navbar';
import Library from './Game/Library';
import AddGame from './Game/AddGame';
import GameDetail from './Game/GameDetail';
import Wishlist from './Wishlist/Wishlist';
import Activity from './Activity/Activity';
import Settings from './Settings/Settings';

function App() {
  return (
    <div className="app">
      <Navbar />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Library />} />
          <Route path="/add" element={<AddGame />} />
          <Route path="/game/:id" element={<GameDetail />} />
          <Route path="/wishlist" element={<Wishlist />} />
          <Route path="/activity" element={<Activity />} />
          <Route path="/settings/*" element={<Settings />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
