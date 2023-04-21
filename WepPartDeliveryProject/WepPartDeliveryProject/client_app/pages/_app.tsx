import Layout from '@/components/Layout'
import '@/styles/globals.css'
import type { AppProps } from 'next/app'
import 'bootstrap/dist/css/bootstrap.min.css';
import { SSRProvider } from 'react-bootstrap';

const App = ({ Component, pageProps }: AppProps) => {

  return (
    <SSRProvider>
        <Layout>
          <Component {...pageProps} />
        </Layout>
    </SSRProvider>
    )
}

export default App;