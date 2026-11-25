import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { SesionProvider } from './SesionContext.jsx'
import Layout from './componentes/Layout.jsx'
import RutaProtegida from './componentes/RutaProtegida.jsx'
import Alimentos from './paginas/Alimentos.jsx'
import Cuenta from './paginas/Cuenta.jsx'
import Dashboard from './paginas/Dashboard.jsx'
import Documentos from './paginas/Documentos.jsx'
import Historial from './paginas/Historial.jsx'
import Login from './paginas/Login.jsx'
import Perfil from './paginas/Perfil.jsx'
import Permisos from './paginas/Permisos.jsx'
import RecuperarContrasena from './paginas/RecuperarContrasena.jsx'
import ReenviarActivacion from './paginas/ReenviarActivacion.jsx'
import Registro from './paginas/Registro.jsx'
import RestablecerContrasena from './paginas/RestablecerContrasena.jsx'
import Usuarios from './paginas/Usuarios.jsx'

// Enrutador principal de la aplicación.
function App() {
  return (
    <BrowserRouter>
      <SesionProvider>
        <Routes>
          <Route element={<Layout />}>
            {/* Páginas públicas */}
            <Route path="/login" element={<Login />} />
            <Route path="/registro" element={<Registro />} />
            <Route path="/reenviar-activacion" element={<ReenviarActivacion />} />
            <Route path="/recuperar-contrasena" element={<RecuperarContrasena />} />
            <Route path="/restablecer-contrasena" element={<RestablecerContrasena />} />

            {/* Páginas que exigen sesión */}
            <Route
              path="/"
              element={
                <RutaProtegida>
                  <Dashboard />
                </RutaProtegida>
              }
            />
            <Route
              path="/alimentos"
              element={
                <RutaProtegida>
                  <Alimentos />
                </RutaProtegida>
              }
            />
            <Route
              path="/documentos"
              element={
                <RutaProtegida>
                  <Documentos />
                </RutaProtegida>
              }
            />
            <Route
              path="/cuenta"
              element={
                <RutaProtegida>
                  <Cuenta />
                </RutaProtegida>
              }
            />
            <Route
              path="/historial"
              element={
                <RutaProtegida>
                  <Historial />
                </RutaProtegida>
              }
            />
            <Route
              path="/permisos"
              element={
                <RutaProtegida>
                  <Permisos />
                </RutaProtegida>
              }
            />
            <Route
              path="/usuarios"
              element={
                <RutaProtegida soloAdmin>
                  <Usuarios />
                </RutaProtegida>
              }
            />
            <Route
              path="/perfil"
              element={
                <RutaProtegida>
                  <Perfil />
                </RutaProtegida>
              }
            />
          </Route>
        </Routes>
      </SesionProvider>
    </BrowserRouter>
  )
}

export default App
