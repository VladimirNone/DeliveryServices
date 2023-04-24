import '@/styles/globals.css'
import type { AppProps } from 'next/app'
import 'bootstrap/dist/css/bootstrap.min.css';
import { SSRProvider } from 'react-bootstrap';
import { CookiesProvider } from 'react-cookie';
import Layout from '@/components/structure/Layout';

const App = ({ Component, pageProps }: AppProps) => {

  return (
    <SSRProvider>
        <Layout>
          <CookiesProvider>
            <Component {...pageProps} />
          </CookiesProvider>
        </Layout>
    </SSRProvider>
    )
}

export default App;