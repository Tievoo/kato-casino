import { createRoot } from 'react-dom/client'
import './index.css'
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router';
import { Home as BlackjackHome } from './pages/blackjack/home.tsx';
import { Room as BlackjackRoom } from './pages/blackjack/room.tsx';

const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/blackjack" replace />,
  },

  {
    path: "/blackjack",
    element: <BlackjackHome />,
  },
  {
    path: "/blackjack/room/:roomId",
    element: <BlackjackRoom />,
  }
]);

createRoot(document.getElementById('root')!).render(
    <RouterProvider router={router} />
)
