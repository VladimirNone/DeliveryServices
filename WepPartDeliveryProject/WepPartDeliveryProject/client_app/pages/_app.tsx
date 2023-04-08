import Layout from '@/components/Layout'
import '@/styles/globals.css'
import type { AppProps } from 'next/app'
import 'bootstrap/dist/css/bootstrap.min.css';
import { SSRProvider } from 'react-bootstrap';
import { useState } from 'react';
import { NavbarContext } from '@/components/contexts/Navbar-context';

const App = ({ Component, pageProps }: AppProps) => {
  const [isAdmin, setIsAdmin] = useState(true);
  const toggleIsAdmin = ()=>{ setIsAdmin(!isAdmin); }

  return (
    <SSRProvider>
      <NavbarContext.Provider value = {{ isAdmin, toggleIsAdmin}}>
        <Layout>
          <Component {...pageProps} />
        </Layout>
      </NavbarContext.Provider >
    </SSRProvider>
    )
}

export default App;