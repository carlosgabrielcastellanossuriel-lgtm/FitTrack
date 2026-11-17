import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Layout from './componentes/Layout.jsx'
import Inicio from './paginas/Inicio.jsx'

// Enrutador principal de la aplicación.
function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Inicio />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
